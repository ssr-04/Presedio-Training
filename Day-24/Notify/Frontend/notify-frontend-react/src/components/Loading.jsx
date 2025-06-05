import React from 'react';

function Loading({ message = "Loading..." }) {
  return (
    <div style={{ textAlign: 'center', padding: '20px', fontSize: '1.2em' }}>
      <p>{message}</p>
      <div className="spinner"></div>
    </div>
  );
}

export default Loading;
