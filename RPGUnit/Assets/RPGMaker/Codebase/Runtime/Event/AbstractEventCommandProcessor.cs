using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Event
{
    public abstract class AbstractEventCommandProcessor
    {
        protected AbstractEventCommandChainLauncher EventCommandChainLauncher;
        protected string                            EventId;
        protected Action                            SendBackToLauncher;
        protected Action<string>                    SendBackToLauncherEventId;
        protected bool                              _skipWait;

        /// <summary>
        /// イベントを実行する
        /// </summary>
        /// <param name="eventCommandChainLauncher"></param>
        /// <param name="eventID"></param>
        /// <param name="command"></param>
        /// <param name="callback"></param>
        public void Invoke(
            AbstractEventCommandChainLauncher eventCommandChainLauncher,
            string eventID,
            EventDataModel.EventCommand command,
            Action<string> callback
        ) {
            EventCommandChainLauncher = eventCommandChainLauncher;
            EventId = eventID;
            SendBackToLauncherEventId = callback;
            SendBackToLauncher = SendBackToLauncherProcess;
            Process(eventID, command);
        }

        /// <summary>
        /// 次回のCB処理のウェイトを行わない
        /// </summary>
        protected void SetNoWait() {
            _skipWait = true;
        }

        /// <summary>
        /// 次イベントコマンドへシーケンスを流す
        /// </summary>
        private void SendBackToLauncherProcess() {
            SendBackToLauncherEventId?.Invoke(EventId);
        }

        /// <summary>
        /// 次イベントコマンドへシーケンスを流す（ウェイト実行）
        /// </summary>
        private void SendBackToLauncherProcessAfterWait() {
            SendBackToLauncherEventId?.Invoke(EventId);
        }

        /// <summary>
        /// イベント実行処理
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="command"></param>
        protected virtual void Process(string eventID, EventDataModel.EventCommand command) {
            // to override
            EventId = eventID;

            // 任意のタイミング（次のコマンドへ進むタイミング）でSendBackToLauncherをinvokeすること
            SendBackToLauncherEventId?.Invoke(eventID);
        }

        /// <summary>
        /// イベント実行処理 選択肢用
        /// </summary>
        /// <param name="eventCommandChainLauncher"></param>
        /// <param name="eventID"></param>
        /// <param name="command"></param>
        /// <param name="callback"></param>
        /// <param name="eventCommands"></param>
        public void Invoke(
            AbstractEventCommandChainLauncher eventCommandChainLauncher,
            string eventID,
            EventDataModel.EventCommand command,
            Action<string> callback,
            List<EventDataModel.EventCommand> eventCommands
        ) {
            EventCommandChainLauncher = eventCommandChainLauncher;
            EventId = eventID;
            SendBackToLauncherEventId = callback;
            SendBackToLauncher = SendBackToLauncherProcess;
            Process(eventID, command, eventCommands);
        }

        /// <summary>
        /// イベント実行処理 選択肢用
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="command"></param>
        /// <param name="eventCommands"></param>
        protected virtual void Process(
            string eventID,
            EventDataModel.EventCommand command,
            List<EventDataModel.EventCommand> eventCommands
        ) {
            // to override
            EventId = eventID;

            // 任意のタイミング（次のコマンドへ進むタイミング）でSendBackToLauncherをinvokeすること
            SendBackToLauncherEventId?.Invoke(eventID);
        }
    }
}