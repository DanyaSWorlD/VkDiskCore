using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkDiskCore.DataBase.Actions;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace VkDiskCore.Utility
{
    public static class UpdateHandler
    {
        private static bool _stop;
        private static readonly object Locker = new object();

        public delegate void MessagesChangedDelegate(List<Message> messages);
        public static event MessagesChangedDelegate OnMesagesChanged;

        //public delegate void DocumentsChangeDelegate(List<Document> docs);
        //public static event DocumentsChangeDelegate OnDocsChanged;

        public static event DocumentDbActions.DocumentsChangedDelegate OnDocsChanged;

        public static bool started;

        public static void Start()
        {
            lock (Locker)
            {
                if (!started)
                {
                    Task.Factory.StartNew(Work);
                    Task.Factory.StartNew(WorkDocs);
                }

                started = true;
            }

        }

        private static void Work()
        {
            var server = VkDisk.VkApi.Messages.GetLongPollServer(needPts: true);
            var newPts = server.Pts ?? 0;
            var ts = ulong.Parse(server.Ts);

            while (!_stop)
            {
                try
                {
                    var history = VkDisk.VkApi.Messages.GetLongPollHistory(new MessagesGetLongPollHistoryParams
                    {
                        Pts = newPts,
                        Ts = ts
                    });

                    newPts = history.NewPts;

                    if (history.Messages != null)
                        if (history.Messages.Count > 0)
                            OnMesagesChanged?.Invoke(history.Messages.ToList());

                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }

        private static void WorkDocs()
        {
            DocumentDbActions.DocumentsChanged += DocsUpdated;
            while (!_stop)
            {
                DocumentDbActions.Sync();

                Thread.Sleep(5000);
            }
        }

        private static void DocsUpdated(IEnumerable<Document> add, IEnumerable<long> delete)
        {
            OnDocsChanged?.Invoke(add, delete);
        }

        public static void Stop(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _stop = true;
        }
    }
}
