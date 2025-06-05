
import React, { useState } from 'react';

function DocumentList({ documents, backendApiBaseUrl, getAccessToken, loadDocuments, currentUserRole }) {
  const [downloadMessage, setDownloadMessage] = useState('');

  const handleDownload = async (documentId, originalFileName) => {
    setDownloadMessage('');
    const token = await getAccessToken();
    if (!token) {
      setDownloadMessage('Authentication token not available.');
      return;
    }

    try {
      const response = await fetch(`${backendApiBaseUrl}/api/documents/${documentId}/download`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      if (!response.ok) {
        const errorText = await response.text();
        setDownloadMessage(`Download failed: ${response.statusText} - ${errorText}`);
        console.error('Download error:', response.status, response.statusText, errorText);
        return;
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = originalFileName;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
      setDownloadMessage('Download initiated.');

    } catch (error) {
      setDownloadMessage('Network error during download.');
      console.error('Network error during download:', error);
    }
  };

  const handleDelete = async (documentId, title) => {
    if (!window.confirm(`Are you sure you want to delete "${title}"?`)) {
      return;
    }
    const token = await getAccessToken();
    if (!token) {
      alert('Authentication token not available.');
      return;
    }

    try {
      const response = await fetch(`${backendApiBaseUrl}/api/documents/${documentId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });

      if (response.ok) {
        alert(`Document "${title}" deleted successfully.`);
        loadDocuments(); // Refresh the list after deletion
      } else {
        const errorText = await response.text();
        alert(`Failed to delete document: ${response.statusText} - ${errorText}`);
        console.error('Delete error:', response.status, response.statusText, errorText);
      }
    } catch (error) {
      alert('Network error during deletion.');
      console.error('Network error during deletion:', error);
    }
  };


  return (
    <div className="document-list-section">
      <h2>Available Documents</h2>
      {downloadMessage && <p className="download-message">{downloadMessage}</p>}
      {documents.length === 0 ? (
        <p>No documents uploaded yet.</p>
      ) : (
        <ul id="document-list">
          {documents.map(doc => (
            <li key={doc.id}>
              <span>
                <strong>{doc.title}</strong> ({doc.originalFileName}, {(doc.fileSize / 1024).toFixed(2)} KB)
                uploaded by {doc.uploadedBy || 'Unknown'} on {new Date(doc.uploadedDate).toLocaleDateString()}
              </span>
              <div>
                <button onClick={() => handleDownload(doc.id, doc.originalFileName)}>Download</button>
                {currentUserRole === 'Admin' && (
                  <button style={{ backgroundColor: '#dc3545', marginLeft: '10px' }} onClick={() => handleDelete(doc.id, doc.title)}>Delete</button>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default DocumentList;