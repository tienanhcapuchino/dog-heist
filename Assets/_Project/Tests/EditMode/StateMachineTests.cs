using System;
using System.Collections.Generic;
using DogHeist.Core.FSM;
using NUnit.Framework;

namespace DogHeist.Tests.EditMode
{
    public sealed class StateMachineTests
    {
        [Test]
        public void ChangeState_ExitsPreviousBeforeEnteringNext()
        {
            var log = new List<string>();
            var machine = new StateMachine();
            var first = new RecordingState("A", log);
            var second = new RecordingState("B", log);

            machine.ChangeState(first);
            machine.ChangeState(second);

            CollectionAssert.AreEqual(new[] { "A.Enter", "A.Exit", "B.Enter" }, log);
            Assert.AreSame(second, machine.CurrentState);
        }

        [Test]
        public void ChangeState_ToCurrentState_DoesNothing()
        {
            var log = new List<string>();
            var machine = new StateMachine();
            var state = new RecordingState("A", log);

            machine.ChangeState(state);
            machine.ChangeState(state);

            CollectionAssert.AreEqual(new[] { "A.Enter" }, log);
        }

        [Test]
        public void Tick_ForwardsToCurrentState()
        {
            var log = new List<string>();
            var machine = new StateMachine();
            machine.ChangeState(new RecordingState("A", log));

            machine.Tick(0.016f);

            CollectionAssert.AreEqual(new[] { "A.Enter", "A.Tick" }, log);
        }

        [Test]
        public void ChangeState_WithNull_Throws()
        {
            var machine = new StateMachine();

            Assert.Throws<ArgumentNullException>(() => machine.ChangeState(null));
        }

        private sealed class RecordingState : IState
        {
            private readonly string _name;
            private readonly List<string> _log;

            public RecordingState(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void Enter() => _log.Add($"{_name}.Enter");

            public void Tick(float deltaTime) => _log.Add($"{_name}.Tick");

            public void Exit() => _log.Add($"{_name}.Exit");
        }
    }
}
