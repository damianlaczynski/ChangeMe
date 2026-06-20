import packageJson from '../../package.json';

export const environment = {
  production: true,
  appVersion: packageJson.version,
  apiUrl: 'http://localhost:5000/api/v1'
};
