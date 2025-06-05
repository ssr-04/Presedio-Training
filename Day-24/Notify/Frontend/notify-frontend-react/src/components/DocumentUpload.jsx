import React, { useState } from 'react';

function DocumentUpload({ backendApiBaseUrl, getAccessToken, onDocumentUploaded }) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [file, setFile] = useState(null);
  const [uploadMessage, setUploadMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setUploadMessage('');
    setLoading(true);

    if (!file || !title) {
      setUploadMessage('Please provide a title and select a file.');
      setLoading(false);
      return;
    }

    const token = await getAccessToken();
    if (!token) {
      setUploadMessage('Authentication token not available.');
      setLoading(false);
      return;
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('title', title);
    if (description) {
      formData.append('description', description);
    }

    try {
      const response = await fetch(`${backendApiBaseUrl}/api/documents/upload`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
        body: formData,
      });

      if (response.ok) {
        const result = await response.json();
        setUploadMessage(`Document "${result.title}" uploaded successfully!`);
        setTitle('');
        setDescription('');
        setFile(null); // Clear file input
        e.target.reset(); // Reset form for file input
        onDocumentUploaded(); // Notify parent to reload documents
      } else {
        const errorText = await response.text();
        setUploadMessage(`Upload failed: ${response.statusText} - ${errorText}`);
        console.error('Upload error:', response.status, response.statusText, errorText);
      }
    } catch (error) {
      setUploadMessage('Network error during upload.');
      console.error('Network error during upload:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="document-upload-section">
      <h2>Upload New Document</h2>
      <form onSubmit={handleSubmit}>
        <label htmlFor="document-title">Document Title:</label>
        <input
          type="text"
          id="document-title"
          placeholder="Enter document title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
        /><br />

        <label htmlFor="document-description">Description (Optional):</label>
        <textarea
          id="document-description"
          rows="3"
          placeholder="Enter document description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        ></textarea><br />

        <label htmlFor="document-file">Select File:</label>
        <input
          type="file"
          id="document-file"
          onChange={(e) => setFile(e.target.files[0])}
          required
        /><br />

        <button type="submit" disabled={loading}>
          {loading ? 'Uploading...' : 'Upload Document'}
        </button>
      </form>
      {uploadMessage && <p className="upload-message">{uploadMessage}</p>}
    </div>
  );
}

export default DocumentUpload;
