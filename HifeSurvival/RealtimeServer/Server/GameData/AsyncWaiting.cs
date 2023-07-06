using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// async 함수 내에서 신호가 들어오기 전에는 await 상태에서 기다리기 위한 클래스
    /// </summary>
    public sealed class AsyncWaiting
    {
        private readonly int _milliSecondsDelay;
        private bool _signal;

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="inMilliSecondsDelay"></param>
        public AsyncWaiting(int inMilliSecondsDelay = 33)
        {
            _milliSecondsDelay = inMilliSecondsDelay;
        }

        /// <summary>
        /// Waiting 취소
        /// </summary>
        public void Signal()
        {
            _signal = true;
        }

        /// <summary>
        /// Signal 신호가 들어올 때까지 대기
        /// </summary>
        /// <returns></returns>
        public async Task Wait()
        {
            while (!_signal)
            {
                await Task.Delay(_milliSecondsDelay);
            }

            _signal = false;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Reset()
        {
            _signal = false;
        }
    }
}