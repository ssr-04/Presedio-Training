import React, { useState, useEffect, useCallback } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import './App.css'; 


import AuthStatus from './components/AuthStatus';
import DocumentUpload from './components/DocumentUpload';
import DocumentList from './components/DocumentList';
import Notifications from './components/Notifications';
import Loading from './components/Loading'; 

import { useSignalR } from './hooks/useSignalR';

function App({ backendApiBaseUrl, auth0Audience }) {
  const {
    isAuthenticated,
    user,
    isLoading: authLoading,
    loginWithRedirect,
    logout,
    getAccessTokenSilently
  } = useAuth0();

  const [currentUserRole, setCurrentUserRole] = useState(null);
  const [documents, setDocuments] = useState([]); 
  const [notifications, setNotifications] = useState([]); 

  // Callback to get the access token for API calls and SignalR
  const getAccessToken = useCallback(async () => {
    if (!isAuthenticated) return null;
    try {
      const token = await getAccessTokenSilently({
        authorizationParams: {
          audience: auth0Audience 
        }
      });

      return token;
    } catch (error) {
      console.error("Error getting access token silently:", error);

      if (error.error === 'login_required' || error.error === 'consent_required') {
        logout({ logoutParams: { returnTo: window.location.origin } });
      }
      return null;
    }
  }, [isAuthenticated, getAccessTokenSilently, logout, backendApiBaseUrl, auth0Audience]);


  useEffect(() => {
    const fetchUserRole = async () => {
      if (!isAuthenticated) {
        setCurrentUserRole(null);
        return;
      }
      const token = await getAccessToken();
      if (!token) {
        setCurrentUserRole(null);
        return;
      }

      try {
        const response = await fetch(`${backendApiBaseUrl}/api/users/me`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
        if (!response.ok) {
          console.error("Failed to fetch user profile:", response.status, response.statusText);
          setCurrentUserRole(null);
          return;
        }
        const userData = await response.json();
        setCurrentUserRole(userData.role);
        console.log("Fetched user role:", userData.role);
      } catch (error) {
        console.error("Error fetching user role from backend:", error);
        setCurrentUserRole(null);
      }
    };

    fetchUserRole();
  }, [isAuthenticated, getAccessToken, backendApiBaseUrl]);

  
  const loadDocuments = useCallback(async () => {
    const token = await getAccessToken();
    if (!token) {
      setDocuments([]);
      return;
    }
    try {
      const response = await fetch(`${backendApiBaseUrl}/api/documents`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!response.ok) {
        console.error("Failed to load documents:", response.status, response.statusText);
        setDocuments([]);
        return;
      }
      const data = await response.json();
      setDocuments(data);
    } catch (error) {
      console.error("Error loading documents:", error);
      setDocuments([]);
    }
  }, [getAccessToken, backendApiBaseUrl]);

  // SignalR setup
  const signalRConnection = useSignalR(`${backendApiBaseUrl}/notificationhub`, getAccessToken);

  useEffect(() => {
    if (signalRConnection) {
      signalRConnection.on("ReceiveDocumentNotification", (document) => {
        console.log("New document notification received:", document);
        setNotifications(prev => [{ id: document.id, message: `New document "${document.title}" uploaded by ${document.uploadedBy} on ${new Date(document.uploadedDate).toLocaleString()}.` }, ...prev]);
        setDocuments(prev => [document, ...prev]); 
      });

      
      signalRConnection.onreconnecting(error => {
        console.warn(`SignalR connection lost. Reconnecting... ${error}`);
        setNotifications(prev => [{ id: Date.now(), message: "SignalR: Reconnecting..." }, ...prev]);
      });

      signalRConnection.onreconnected(connectionId => {
        console.log(`SignalR reconnected. Connection ID: ${connectionId}`);
        setNotifications(prev => [{ id: Date.now(), message: "SignalR: Reconnected!" }, ...prev]);
        loadDocuments(); 
      });

      signalRConnection.onclose(error => {
        console.error(`SignalR connection closed. ${error}`);
        setNotifications(prev => [{ id: Date.now(), message: `SignalR: Disconnected. ${error?.message || ''}` }, ...prev]);
      });
    }
  }, [signalRConnection, loadDocuments]); // Depend on signalRConnection and loadDocuments

  
  useEffect(() => {
    if (isAuthenticated) {
      loadDocuments();
    } else {
      setDocuments([]); // Clear documents if not authenticated
    }
  }, [isAuthenticated, loadDocuments]);

  if (authLoading) {
    return <Loading message="Authenticating..." />;
  }

  return (
    <div className="container">
      <h1>Notify - Document Sharing for Internal Teams</h1>

      <AuthStatus
        isAuthenticated={isAuthenticated}
        user={user}
        role={currentUserRole}
        login={loginWithRedirect}
        logout={logout}
      />

      {isAuthenticated ? (
        <>
          {currentUserRole === 'Admin' && (
            <DocumentUpload
              backendApiBaseUrl={backendApiBaseUrl}
              getAccessToken={getAccessToken}
              onDocumentUploaded={loadDocuments} // Reload documents after upload
            />
          )}

          <DocumentList
            documents={documents}
            backendApiBaseUrl={backendApiBaseUrl}
            getAccessToken={getAccessToken}
            loadDocuments={loadDocuments} // Pass for potential refresh
            currentUserRole={currentUserRole} // Pass role for delete button
          />

          <Notifications notifications={notifications} />
        </>
      ) : (
        <p>Please log in to view documents and notifications.</p>
      )}
    </div>
  );
}

export default App;