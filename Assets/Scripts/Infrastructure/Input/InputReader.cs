using System;
using System.Collections.Generic;
using Quadrop.Game;
using UnityEngine.InputSystem;

namespace Quadrop.Infrastructure
{
    /// <summary>
    /// Input System 액션을 GameCommand로 바꿔 게임에 넣는다.
    /// 키 바인딩은 코드에서 만들고, 좌우 이동의 자동 반복(DAS/ARR)도 여기서 처리한다.
    /// </summary>
    public sealed class InputReader : IDisposable
    {
        private const int MaxRepeatsPerTick = 8;

        private readonly struct CommandBinding
        {
            public InputAction Action { get; }
            public Action<InputAction.CallbackContext> Handler { get; }

            public CommandBinding(InputAction action, Action<InputAction.CallbackContext> handler)
            {
                Action = action;
                Handler = handler;
            }
        }

        private readonly IGameCommandSink _sink;
        private readonly InputTuning _tuning;
        private readonly InputActionMap _map;
        private readonly InputAction _moveLeft;
        private readonly InputAction _moveRight;
        private readonly InputAction _softDrop;
        private readonly List<CommandBinding> _bindings = new List<CommandBinding>();

        private int _heldDirection;
        private int _lastPressedDirection;
        private float _repeatTimer;
        private bool _isRepeating;
        private bool _isDisposed;

        public InputReader(IGameCommandSink sink, InputTuning tuning)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _map = new InputActionMap("Quadrop");

            _moveLeft = AddButton("MoveLeft", "<Keyboard>/leftArrow", "<Gamepad>/dpad/left");
            _moveRight = AddButton("MoveRight", "<Keyboard>/rightArrow", "<Gamepad>/dpad/right");
            _softDrop = AddButton("SoftDrop", "<Keyboard>/downArrow", "<Gamepad>/dpad/down");

            BindCommand(AddButton("RotatePiece", "<Keyboard>/z", "<Keyboard>/upArrow", "<Gamepad>/buttonSouth", "<Gamepad>/buttonEast"),
                GameCommand.RotatePiece);
            BindCommand(AddButton("HardDrop", "<Keyboard>/space", "<Gamepad>/dpad/up"),
                GameCommand.HardDrop);
            BindCommand(AddButton("Hold", "<Keyboard>/c", "<Gamepad>/leftShoulder", "<Gamepad>/rightShoulder"),
                GameCommand.Hold);
            BindCommand(AddButton("RotateBoardCcw", "<Keyboard>/a", "<Gamepad>/leftTrigger", "<Gamepad>/buttonWest"),
                GameCommand.RotateBoardCounterClockwise);
            BindCommand(AddButton("RotateBoardCw", "<Keyboard>/d", "<Gamepad>/rightTrigger", "<Gamepad>/buttonNorth"),
                GameCommand.RotateBoardClockwise);
            BindCommand(AddButton("Pause", "<Keyboard>/escape", "<Keyboard>/enter", "<Gamepad>/start"),
                GameCommand.Pause);
            BindCommand(AddButton("Restart", "<Keyboard>/r", "<Gamepad>/select"),
                GameCommand.Restart);

            _softDrop.started += OnSoftDropStarted;
            _softDrop.canceled += OnSoftDropCanceled;
            _map.Enable();
        }

        /// <summary>매 프레임 호출: 좌우 키를 누르고 있으면 DAS 뒤 ARR 간격으로 이동 명령을 반복한다.</summary>
        public void Tick(float deltaSeconds)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_moveLeft.WasPressedThisFrame())
            {
                _lastPressedDirection = -1;
            }
            if (_moveRight.WasPressedThisFrame())
            {
                _lastPressedDirection = 1;
            }

            int direction = ReadDirection();
            if (direction != _heldDirection)
            {
                _heldDirection = direction;
                _repeatTimer = 0f;
                _isRepeating = false;
                if (direction != 0)
                {
                    EmitMove(direction);
                }
                return;
            }

            if (direction == 0)
            {
                return;
            }

            _repeatTimer += deltaSeconds;
            int repeats = 0;
            float threshold = _isRepeating ? _tuning.ArrSeconds : _tuning.DasSeconds;
            while (_repeatTimer >= threshold && repeats < MaxRepeatsPerTick)
            {
                _repeatTimer -= threshold;
                _isRepeating = true;
                threshold = _tuning.ArrSeconds;
                EmitMove(direction);
                repeats++;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            for (int i = 0; i < _bindings.Count; i++)
            {
                _bindings[i].Action.performed -= _bindings[i].Handler;
            }
            _bindings.Clear();
            _softDrop.started -= OnSoftDropStarted;
            _softDrop.canceled -= OnSoftDropCanceled;
            _map.Disable();
            _map.Dispose();
        }

        private int ReadDirection()
        {
            bool left = _moveLeft.IsPressed();
            bool right = _moveRight.IsPressed();
            if (left && right)
            {
                return _lastPressedDirection;
            }
            if (left)
            {
                return -1;
            }
            if (right)
            {
                return 1;
            }
            return 0;
        }

        private void EmitMove(int direction)
        {
            _sink.Enqueue(direction < 0 ? GameCommand.MoveLeft : GameCommand.MoveRight);
        }

        private InputAction AddButton(string name, params string[] bindings)
        {
            InputAction action = _map.AddAction(name, InputActionType.Button);
            for (int i = 0; i < bindings.Length; i++)
            {
                action.AddBinding(bindings[i]);
            }
            return action;
        }

        private void BindCommand(InputAction action, GameCommand command)
        {
            Action<InputAction.CallbackContext> handler = context => _sink.Enqueue(command);
            action.performed += handler;
            _bindings.Add(new CommandBinding(action, handler));
        }

        private void OnSoftDropStarted(InputAction.CallbackContext context)
        {
            _sink.SetSoftDrop(true);
        }

        private void OnSoftDropCanceled(InputAction.CallbackContext context)
        {
            _sink.SetSoftDrop(false);
        }
    }
}
