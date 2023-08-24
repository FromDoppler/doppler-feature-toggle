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

#if (!NET35 && !NET40)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.FeatureToggle.Internal
{
    /// <summary>
    /// Alternative for Timer
    /// </summary>
    public class TimerAlternative : IDisposable
    {
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Executed a task recurrently
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        /// <param name="dueTime"></param>
        public TimerAlternative(Func<object, Task> action, object state, TimeSpan dueTime, TimeSpan period)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(dueTime, _tokenSource.Token);
                    while (!_tokenSource.Token.IsCancellationRequested)
                    {
                        await action(state);
                        await Task.Delay(period, _tokenSource.Token);
                    }
                }
                catch (TaskCanceledException) when (_tokenSource.IsCancellationRequested)
                {
                    // Expected exception on dispose
                }
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tokenSource.Cancel();
        }
    }
}
#endif