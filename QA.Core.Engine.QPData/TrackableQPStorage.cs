using System;
using QA.Core.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QA.Core.Logger;
#pragma warning disable 1591

namespace QA.Core.Engine.QPData
{
    public class TrackableQPStorage2 : QPStorage, IDisposable
    {
        private Timer _job;
        private static readonly string PollingCacheKey = "TrackableQPStorage<>";
        private VersionInfo _currentVersion = null;

        private IVersionedCacheProvider _versionedCacheProvider;
        private ICacheProvider _cacheProvider;
        private TrackableStorageConfig _config;

        public TrackableStorageConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        protected IEnumerable<string> CacheTags { get; set; }

        private ILogger _logger;
        private volatile bool _isBusy = false;

        public TrackableQPStorage2(AbstractItemLoader loader,
            ICacheProvider cacheProvider,
            IVersionedCacheProvider versionedCacheProvider,
            ILogger logger)
            : base(loader)
        {
            _versionedCacheProvider = versionedCacheProvider;
            _cacheProvider = cacheProvider;

            _logger = logger;
        }

        public void Initialize(TrackableStorageConfig config, IEnumerable<string> cacheTags)
        {
            _config = config;
            CacheTags = cacheTags;
        }

        public void Start()
        {
            _logger.Info(() => $"starting timer with period: {_config.PollPeriod}");
            _job = new Timer(OnUpdate, new object(), TimeSpan.FromMilliseconds(0), _config.PollPeriod);
            _logger.Info(() => "timer started");
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
            if (_job != null)
                _job.Dispose();

            base.Dispose();
        }
    }
}
