import React from 'react';

function AuthStatus({ isAuthenticated, user, role, login, logout }) {
  return (
    <div className="auth-status">
      <p><strong>Status:</strong> <span id="user-status">{isAuthenticated ? 'Authenticated' : 'Not Authenticated'}</span></p>
      {isAuthenticated && user && (
        <p id="user-info">
          <strong>Welcome,</strong> <span id="user-email">{user.email || user.name}</span> (<span id="user-role">{role || 'Loading Role...'}</span>)
        </p>
      )}
      {!isAuthenticated && <button onClick={() => login()}>Log In</button>}
      {isAuthenticated && <button id="logout-btn" onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>Log Out</button>}
    </div>
  );
}

export default AuthStatus;