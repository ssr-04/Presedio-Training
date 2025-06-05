import React from 'react';

function Notifications({ notifications }) {
  return (
    <div className="notifications-section">
      <h2>Real-time Notifications</h2>
      <ul id="notification-list">
        {notifications.length === 0 ? (
          <li>Waiting for new document notifications...</li>
        ) : (
          notifications.map((notif, index) => (
            <li key={notif.id || index} className="notification-item">{notif.message}</li>
          ))
        )}
      </ul>
    </div>
  );
}

export default Notifications;