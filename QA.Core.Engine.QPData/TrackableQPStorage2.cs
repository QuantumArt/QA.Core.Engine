using System;
using QA.Core.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Core.Engine.QPData
{
    public class TrackableQPStorage : QPStorage, IDisposable
    {
        private readonly Timer _job;
        private static readonly string PollingCacheKey = "TrackableQPStorage<>";
        private VersionInfo _currentVersion = null;

        private readonly IVersionedCacheProvider _versionedCacheProvider;
        private readonly ICacheProvider _cacheProvider;
        private TrackableStorageConfig _config;

        public TrackableStorageConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        protected IEnumerable<string> CacheTags { get; set; }

        private ILogger _logger;
        private volatile bool _isBusy = false;

        public TrackableQPStorage(AbstractItemLoader loader,
            ICacheProvider cacheProvider,
            IVersionedCacheProvider versionedCacheProvider,
            ILogger logger)
            : base(loader)
        {
            _logger = logger;
            _versionedCacheProvider = versionedCacheProvider;
            _cacheProvider = cacheProvider;
            _logger.Info(_ => $"###creating timer ###");
            _job = new Timer(OnUpdate, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Initialize(TrackableStorageConfig config, IEnumerable<string> cacheTags)
        {
            _config = config;
            CacheTags = cacheTags;
        }

        public void Start()
        {
            _logger.Info(_ => $"### changing timer with period: {_config.PollPeriod} ###");
            _job.Change(_config.PollPeriod, _config.PollPeriod);
            _logger.Info(_ => "### timer started");
        }

        public override void ReloadAll()
        {
            Reload(true);
            _currentVersion = _versionedCacheProvider.GetOrAdd(PollingCacheKey, GetKeys(),
                    _config.CacheInterval, VersionInfo.GetVersion)
                    .Clone();
        }

        protected string[] GetKeys()
        {

            if (CacheTags == null)
            {
                throw new InvalidOperationException("Property CacheTags has not been set.");
            }

            return CacheTags.ToArray();
        }

        private void Reload(bool checkIfLoaded = false)
        {
            if (!checkIfLoaded || Model.Root == null)
            {
                Loader.LoadAll(Model);
            }
        }

        protected void OnUpdate(object state)
        {
            Update();
        }

        public void Update(bool forceUpdate = false)
        {
            if (_isBusy)
                return;

            lock (_job)
            {
                _isBusy = true;
                try
                {
                    var cacheProvider = _cacheProvider;
                    var versionedCacheProvider = _versionedCacheProvider;

                    VersionInfo version;

                    if (forceUpdate)
                    {
                        version = VersionInfo.GetVersion();
                        _versionedCacheProvider.Add(version, PollingCacheKey, GetKeys(), _config.CacheInterval);
                        version = version.Clone();
                    }
                    else
                    {
                        version = _versionedCacheProvider.GetOrAdd(PollingCacheKey, GetKeys(),
                            _config.CacheInterval, VersionInfo.GetVersion)
                            .Clone();
                    }

                    if (!version.Equals(_currentVersion) || Model.Root == null)
                    {
                        _logger.Debug("Site map is being reloaded. ");
                        Reload();
                        _currentVersion = version;
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Trackable storage exception", ex);
                }
                finally
                {
                    _isBusy = false;
                }
            }
        }

        public void Stop()
        {
            if (_job != null)
                _job.Dispose();
        }
        public override void Dispose()
        {
            _logger.Info("### Disposing of TrackableQPStorage");
            if (_job != null)
                _job.Dispose();

            _logger.Info("### TrackableQPStorage is disposed");
            base.Dispose();
        }
    }

    public class TrackableStorageConfig
    {
        public bool IsEnabled { get; set; }
        public bool ContentWatcherEnabled { get; set; }
        public TimeSpan PollPeriod { get; set; }
        public TimeSpan CacheInterval { get; set; }
        public TimeSpan WatcherPeriod { get; set; }
    }

    [Serializable]
    public class VersionInfo
    {
        public VersionInfo()
            : this(null)
        {

        }

        public VersionInfo(string version)
        {
            this.Version = version ?? "";
        }

        public static VersionInfo GetVersion()
        {
            var now = DateTime.Now;
            return new VersionInfo(string.Format("{0}_{1}", now, now.Ticks))
            {
                LastUpdated = now
            };
        }

        public static bool operator ==(VersionInfo v1, VersionInfo v2)
        {
            if (v1 != null && v2 != null)
            {
                return v1.Equals(v2);
            }

            return false;
        }

        public static bool operator !=(VersionInfo v1, VersionInfo v2)
        {
            return !(v1 == v2);
        }

        public string Version { get; set; }

        public DateTime LastUpdated { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is VersionInfo)
            {
                return ((VersionInfo)obj).Version == this.Version;
            }

            return base.Equals(obj);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (this.Version).GetHashCode();
        }

        public VersionInfo Clone()
        {
            return (VersionInfo)MemberwiseClone();
        }
    }
}
