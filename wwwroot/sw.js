const CACHE_NAME = 'cafeteria-ucol-v1';
const ASSETS_TO_CACHE = [
  '/',
  '/index.html',
  '/manifest.json',
  '/favicon.png',
  '/_content/MudBlazor/MudBlazor.min.css',
  '/_content/MudBlazor/MudBlazor.min.js'
];

// Fase de Instalación: Cachear Shell de la Aplicación
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(ASSETS_TO_CACHE);
    })
  );
});

// Fase de Activación: Limpieza de versiones obsoletas
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cache) => {
          if (cache !== CACHE_NAME) {
            return caches.delete(cache);
          }
        })
      );
    })
  );
});

// Estrategia de Red con Respaldo en Caché (Network First)
self.addEventListener('fetch', (event) => {
  if (event.request.method !== 'GET') return;

  event.respondWith(
    fetch(event.request)
      .then((response) => {
        // Clonar y actualizar caché si la respuesta es válida
        if (response.status === 200) {
          const responseClone = response.clone();
          caches.open(CACHE_NAME).then((cache) => {
            cache.put(event.request, responseClone);
          });
        }
        return response;
      })
      .catch(() => {
        // Retornar recurso desde caché en caso de pérdida de conectividad
        return caches.match(event.request);
      })
  );
});