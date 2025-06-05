// notify-frontend-react/src/main.jsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.jsx';
import './index.css';
import { Auth0Provider } from '@auth0/auth0-react';

const backendApiBaseUrl = "https://localhost:7001"; // !!! IMPORTANT: Your actual API URL and PORT !!!

async function getConfig() {
  const response = await fetch('/auth_config.json');
  const config = await response.json();
  return config;
}

getConfig().then(config => {
  ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
      <Auth0Provider
        domain={config.domain}
        clientId={config.clientId}
        authorizationParams={{
          redirect_uri: window.location.origin,
          audience: config.audience // This is the Auth0 API Identifier
        }}
      >
        {/* Pass the auth0Audience prop to App */}
        <App backendApiBaseUrl={backendApiBaseUrl} auth0Audience={config.audience} />
      </Auth0Provider>
    </React.StrictMode>,
  );
});