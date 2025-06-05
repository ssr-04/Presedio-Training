
import { useEffect, useState, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

export function useSignalR(hubUrl, accessTokenFactory) {
  const [connection, setConnection] = useState(null);
  const isConnecting = useRef(false); // Ref to prevent multiple connection attempts

  useEffect(() => {
    // Only attempt to connect if an access token factory is available
    // and we're not already connecting or connected.
    if (!accessTokenFactory || isConnecting.current || (connection && connection.state === signalR.HubConnectionState.Connected)) {
        return;
    }

    isConnecting.current = true; // Mark as connecting

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: async () => {
          const token = await accessTokenFactory(); // Get token from Auth0 hook
          // console.log("SignalR accessTokenFactory called, token:", token ? "Available" : "Not Available");
          return token;
        }
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
            if (retryContext.elapsedMilliseconds < 60000) { // Retry for 1 minute
                return Math.random() * 1000 + 1000; // Random retry delay between 1-2 seconds
            }
            return null; // Stop reconnecting after 1 minute
        }
      })
      .build();

    newConnection.start()
      .then(() => {
        console.log("SignalR Connected!");
        setConnection(newConnection);
        isConnecting.current = false; // Reset connecting state
      })
      .catch(err => {
        console.error("Error connecting to SignalR:", err);
        isConnecting.current = false; // Reset connecting state
        // You might want to handle persistent errors here, e.g., show a message to the user
      });

    // Cleanup function for useEffect
    return () => {
      if (connection) {
        connection.stop()
          .then(() => console.log("SignalR Disconnected!"))
          .catch(err => console.error("Error disconnecting SignalR:", err));
      }
      isConnecting.current = false; // Ensure connecting state is reset on cleanup
    };
  }, [hubUrl, accessTokenFactory]); // Re-run effect if hubUrl or accessTokenFactory changes

  // Return the connection object for use in components, or null if not connected
  return connection;
}