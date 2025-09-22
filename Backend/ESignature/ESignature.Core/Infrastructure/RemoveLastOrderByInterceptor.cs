using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ESignature.Core.Infrastructure
{
    public class RemoveLastOrderByInterceptor : DbCommandInterceptor
    {
        public const string QueryTag = "RemoveLastOrderBy";

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            const string orderBy = "ORDER BY";
            if (command.CommandText.Contains(QueryTag) && command.CommandText.Contains(orderBy))
            {
                int lastOrderBy = command.CommandText.LastIndexOf(orderBy, StringComparison.Ordinal);
                //beware of string manip on memory consumption
                command.CommandText = command.CommandText.Remove(lastOrderBy);
                //command.CommandText += ";";
            }
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}