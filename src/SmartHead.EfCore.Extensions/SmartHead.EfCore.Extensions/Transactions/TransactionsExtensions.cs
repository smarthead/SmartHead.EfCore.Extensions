using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SmartHead.EfCore.Extensions.Transactions
{
    public static class TransactionsExtensions
    {
        public static async Task<bool> RetryAsync(
            this DbContext dbContext, 
            Func<IDbContextTransaction, int, Task<bool>> action, 
            Func<IDbContextTransaction, int, Task<bool>> retryCondition = null,
            int maxRetryCount = 5,
            Func<int, Task> before = null,
            Func<int, Task> after = null,
            Action<Exception> onFailure = null,
            Func<int, Task> onRetryCountExceeded = null)
        {
            if (dbContext.Database.CurrentTransaction == null)
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                return await RetryInvokeAsync(transaction);
            }
            else
            {
                return await RetryInvokeAsync(dbContext.Database.CurrentTransaction);
            }
            
            async Task<bool> RetryInvokeAsync(IDbContextTransaction dbContextTransaction)
            {
                var committed = false;
                
                var retries = Enumerable.Range(1, maxRetryCount)
                    .ToArray();

                foreach (var retry in retries)
                {
                    if (before != null)
                        await before.Invoke(retry);

                    try
                    {
                        if (retry == maxRetryCount)
                            break;

                        if (retryCondition != null
                            && !await retryCondition(dbContextTransaction, retry))
                        {
                            committed = true;
                            break;
                        }

                        committed = await InvokeAction(dbContextTransaction, retry);
                        break;
                    }
                    catch (Exception e)
                    {
                        onFailure?.Invoke(e);
                    }

                    if (after != null)
                        await after.Invoke(retry);
                }

                return committed;
            }

            async Task<bool> InvokeAction(IDbContextTransaction dbContextTransaction, int currentRetry)
            {
                try
                {
                    if (await action(dbContextTransaction, currentRetry))
                    {
                        await dbContextTransaction.CommitAsync();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw;
                }
                return false;
            }
        }
    }
}