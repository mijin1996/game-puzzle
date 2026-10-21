using Quadrop.Core;

namespace Quadrop.Game
{
    // 패턴: Repository — 저장 위치(PlayerPrefs, 파일, 서버)가 바뀌어도 게임 흐름 코드를 고치지 않으려고
    public interface IHighScoreRepository
    {
        /// <summary>저장된 최고 점수. 없거나 읽지 못하면 Fail.</summary>
        Result<int> Load();

        Result<bool> Save(int score);
    }
}
