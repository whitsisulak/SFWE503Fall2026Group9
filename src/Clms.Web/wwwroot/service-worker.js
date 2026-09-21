// Development service worker: intentionally does nothing.
// The real caching logic lives in service-worker.published.js and is only
// swapped in by `dotnet publish`, so local dev never serves stale assets.
self.addEventListener('fetch', () => { });
