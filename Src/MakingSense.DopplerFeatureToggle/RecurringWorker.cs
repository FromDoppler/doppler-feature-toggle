#region License
// Copyright (c) 2017 Doppler Relay Team
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if (NETSTANDARD1_0)
using Timer = MakingSense.DopplerFeatureToggle.Internal.TimerAlternative;
#else
using System.Threading;
#endif
using System.Threading.Tasks;

namespace MakingSense.DopplerFeatureToggle
{
    /// <summary>
    /// Executes an action periodically
    /// </summary>
    /// <remarks>
    /// See ExecutionFailed and ExecutionOmitted events to deal with errors
    /// </remarks>
    public class RecurringWorker : IDisposable
    {
        private readonly Func<Task> _action;
        private readonly Timer _timer;
        private bool _inProgress = false;

        /// <summary>
        /// Event raised when action execution ends with an exception
        /// </summary>
        public event EventHandler<TaskExecutionFailedEventArgs> ExecutionFailed;

        /// <summary>
        /// Event raised when the time  of action execution arrives and the previous task did not finish yet
        /// </summary>
        public event EventHandler ExecutionOmitted;

        /// <summary>
        /// Starts a periodical execution of specified action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        /// <param name="dueTime"></param>
        public RecurringWorker(Func<Task> action, TimeSpan dueTime, TimeSpan period)
        {
            _action = action;
            _timer = new Timer(async _ => await RunAction(), null, dueTime, period);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _timer.Dispose();
        }

        private async Task RunAction()
        {
            if (_inProgress)
            {
                ExecutionOmitted?.Invoke(this, new EventArgs());
                return;
            }

            try
            {
                _inProgress = true;
                await _action();
            }
            catch (Exception e)
            {
                ExecutionFailed?.Invoke(this, new TaskExecutionFailedEventArgs(e));
            }
            finally
            {
                _inProgress = false;
            }
        }
    }

    /// <summary>
    /// EventArgs used when a task execution fails
    /// </summary>
    public class TaskExecutionFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Exception related with task execution failure
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Creates a new TaskExecutionFailedEventArgs instance
        /// </summary>
        /// <param name="exception"></param>
        public TaskExecutionFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
