import { environment } from './environment';

export interface ChangeMeRuntimeConfig {
  apiUrl: string;
}

declare global {
  interface Window {
    __CHANGE_ME_CONFIG__?: ChangeMeRuntimeConfig;
  }
}

/** Development: `environment.development.ts`. Production: `runtime-config.js` (required). */
export function getApiUrl(): string {
  if (!environment.production) {
    return (environment as typeof environment & { apiUrl: string }).apiUrl;
  }

  const url = window.__CHANGE_ME_CONFIG__?.apiUrl?.trim();
  if (!url) {
    throw new Error(
      'Missing window.__CHANGE_ME_CONFIG__.apiUrl. Set public/runtime-config.js or CHANGE_ME_API_URL at container start.'
    );
  }

  return url;
}

function getHubPathFromApiUrl(apiUrl: string): string {
  return apiUrl.replace(/\/api(?:\/v\d+)?\/?$/, '/hubs/notifications');
}

export function getNotificationsHubUrl(): string {
  return getHubPathFromApiUrl(getApiUrl());
}
