using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doppler.FeatureToggle.Internal
{
    /// <summary>
    /// Utilities to simplify Task usage in different frameworks
    /// </summary>
    public static class TaskUtilities
    {
        /// <summary>
        /// Creates a System.Threading.Tasks.Task`1 that's completed successfully with the
        /// specified result.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the task.</typeparam>
        /// <param name="result">The result to store into the completed task.</param>
        /// <returns>The successfully completed task.</returns>
        public static Task<T> FromResult<T>(T result)
#if (HAVE_ASYNC)
            => Task.FromResult(result);
#else
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }
#endif

        private static Task _completedTask = null;

        /// <summary>
        /// Returns a cached already completed task
        /// </summary>
        public static Task CompletedTask => _completedTask ?? (_completedTask = FromResult(true));
    }
}
