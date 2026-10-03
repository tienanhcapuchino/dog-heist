using DogHeist.Core.Match;
using DogHeist.Core.Stats;
using DogHeist.Gameplay.Thief;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DogHeist.Gameplay.Match
{
    /// <summary>
    /// Điều phối một ván: bắt đầu, nhận sự kiện bị bắt hoặc trốn thoát, ghi thành tích, báo kết thúc.
    /// Đây là nơi giữ "quyền quyết định" của ván; khi lên multiplayer, component này chỉ chạy trên server.
    /// </summary>
    public sealed class MatchManager : MonoBehaviour
    {
        [SerializeField] private PlayerRole _localRole = PlayerRole.Thief;
        [SerializeField] private ThiefMotor _thief;
        [SerializeField] private bool _lockCursorDuringPlay = true;

        private StatsService _stats;
        private float _startTime;
        private float _endedDuration;
        private bool _wasThiefSpotted;

        public MatchState State { get; private set; } = MatchState.NotStarted;

        public PlayerStats CurrentStats => _stats?.Current;

        /// <summary>Thời gian ván đã trôi qua; đóng băng khi ván kết thúc.</summary>
        public float ElapsedSeconds => State switch
        {
            MatchState.Playing => Time.time - _startTime,
            MatchState.Ended => _endedDuration,
            _ => 0f,
        };

        private void Awake() => _stats = new StatsService(new JsonFileStatsStorage());

        private void OnEnable()
        {
            MatchEvents.ThiefSpotted += HandleThiefSpotted;
            MatchEvents.ThiefCaught += HandleThiefCaught;
            MatchEvents.ThiefEscaped += HandleThiefEscaped;
        }

        private void OnDisable()
        {
            MatchEvents.ThiefSpotted -= HandleThiefSpotted;
            MatchEvents.ThiefCaught -= HandleThiefCaught;
            MatchEvents.ThiefEscaped -= HandleThiefEscaped;
        }

        private void Start() => BeginMatch();

        public void RestartMatch()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError("Scene hiện tại chưa có trong Build Profiles > Scene List nên không tải lại được.", this);
                return;
            }

            SceneManager.LoadScene(scene.buildIndex);
        }

        private void BeginMatch()
        {
            _startTime = Time.time;
            _wasThiefSpotted = false;
            State = MatchState.Playing;
            SetCursorLocked(_lockCursorDuringPlay);
        }

        private void HandleThiefSpotted()
        {
            if (State == MatchState.Playing)
            {
                _wasThiefSpotted = true;
            }
        }

        private void HandleThiefCaught() => EndMatch(MatchOutcome.ThiefCaught);

        private void HandleThiefEscaped() => EndMatch(MatchOutcome.ThiefEscaped);

        private void EndMatch(MatchOutcome outcome)
        {
            if (State != MatchState.Playing)
            {
                return;
            }

            State = MatchState.Ended;

            if (_thief != null)
            {
                _thief.InputEnabled = false;
            }

            _endedDuration = Time.time - _startTime;
            var result = new MatchResult(_localRole, outcome, _endedDuration, _wasThiefSpotted);
            _stats.Record(result);
            SetCursorLocked(false);
            MatchEvents.RaiseMatchEnded(result, _stats.Current);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
