#!/usr/bin/env python3
"""
Flask API for Intelligent Agentic RAG

This script sets up a Flask server to expose the intelligent
decision-making agent via a REST API endpoint at /api/agentic_qna.
It loads necessary resources (FAISS index, BM25 index, Redis cache)
once upon startup and handles incoming Q&A requests, including basic
input validation and error handling.
"""

import os
import sys
import logging

from flask import Flask, request, jsonify
from flask_cors import CORS


# --- Configure basic logging early ---
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# --- Import necessary components from your rag_agent script ---
# Adjust the sys.path.insert if rag_agent.py is not in the same directory
# as this app.py file. This assumes rag_agent.py is in the same directory.
current_dir = os.path.dirname(os.path.abspath(__file__))
rag_agent_path = os.path.join(current_dir) # Assumes rag_agent.py is in current_dir
if rag_agent_path not in sys.path:
    sys.path.insert(0, rag_agent_path)

try:
    # Attempt to import required items from rag_agent.py
    from rag_agent import (
        load_env_vars, ensure_nltk_resources,
        load_faiss_index_and_metadata, load_bm25_index,
        RedisCacheManager, answer_query_agentic_with_cache,
        REDIS_HOST, REDIS_PORT, REDIS_DB, REDIS_PASSWORD, REDIS_INDEX_NAME,
        FAISS_INDEX_FILE, FAISS_METADATA_FILE, BM25_INDEX_FILE,
        # Assuming VECTOR_DIMENSION is a global constant or easily obtainable
        # If not, you might need to define it here based on your model (Gemini text-embedding-004 is 768)
        # VECTOR_DIMENSION = 768 # Explicitly define if not in rag_agent.py
    )
    # If VECTOR_DIMENSION is not imported, define it
    if 'VECTOR_DIMENSION' not in locals() and 'VECTOR_DIMENSION' not in globals():
         # Fallback: Define based on known Gemini embedding dimension
         VECTOR_DIMENSION = 768
         logger.info(f"VECTOR_DIMENSION not imported from rag_agent.py, defaulting to {VECTOR_DIMENSION}")

except ImportError as e:
    logger.critical(f"Error importing rag_agent components: {e}")
    logger.critical("Please ensure 'rag_agent.py' exists, is in the correct path, and its structure is importable (no top-level CLI block).")
    sys.exit(1)
except Exception as e:
    logger.critical(f"An unexpected error occurred during import from rag_agent.py: {e}")
    sys.exit(1)


# --- Flask Application Setup ---
app = Flask(__name__)
CORS(app) 

# --- Global variables to hold loaded resources ---
# These will be loaded once when the application starts
faiss_index = None
faiss_metadata = None
bm25_index = None
redis_cache_manager = None

# --- Resource Loading Function (Runs Once) ---
def load_resources():
    """
    Loads necessary resources (environment variables, NLTK data,
    FAISS index, metadata, BM25 index, Redis cache manager)
    once before the first request is handled.
    """
    global faiss_index, faiss_metadata, bm25_index, redis_cache_manager

    logger.info("--- Starting resource loading for Flask app ---")

    # Load environment variables first
    logger.info("Loading environment variables...")
    try:
        load_env_vars()
        logger.info("Environment variables loaded.")
    except Exception as e:
        logger.error(f"Error loading environment variables: {e}")
        # System might still function partially, but log the error

    # Ensure NLTK resources are available
    logger.info("Ensuring NLTK resources...")
    try:
        ensure_nltk_resources()
        logger.info("NLTK resources checked/downloaded.")
    except Exception as e:
        logger.error(f"Error ensuring NLTK resources: {e}")
        # NLTK Punkt is used by word_tokenize, which is used by BM25 (optional).
        # A failure here might only impact BM25 if NLTK wasn't already present.

    # Load FAISS Index and Metadata
    logger.info(f"Loading FAISS index from {FAISS_INDEX_FILE} and {FAISS_METADATA_FILE}...")
    try:
        faiss_index, faiss_metadata = load_faiss_index_and_metadata(
            index_path=FAISS_INDEX_FILE,
            metadata_path=FAISS_METADATA_FILE
        )
        if faiss_index is None or faiss_metadata is None:
            logger.critical("Failed to load essential FAISS resources. API will not be able to perform RAG.")
        else:
             logger.info("FAISS resources loaded successfully.")
    except Exception as e:
        logger.critical(f"An unexpected error occurred loading FAISS resources: {e}")
        faiss_index = None # Ensure they are None if loading fails
        faiss_metadata = None

    # Load BM25 Index
    logger.info(f"Loading BM25 index from {BM25_INDEX_FILE}...")
    try:
        bm25_index = load_bm25_index(bm25_path=BM25_INDEX_FILE)
        if bm25_index is None:
            logger.warning("Failed to load BM25 index. Hybrid retrieval will be dense-only.")
        else:
            logger.info("BM25 index loaded successfully.")
    except Exception as e:
        logger.error(f"An unexpected error occurred loading BM25 index: {e}")
        bm25_index = None # Ensure it's None if loading fails


    # Initialize Redis Cache Manager
    logger.info("Initializing Redis Cache Manager...")
    try:
        # Ensure Redis config variables are available from load_env_vars
        # If not, RedisCacheManager uses defaults or you might need to handle this
        redis_cache_manager = RedisCacheManager(
            host=REDIS_HOST,
            port=REDIS_PORT,
            db=REDIS_DB,
            password=REDIS_PASSWORD,
            index_name=REDIS_INDEX_NAME,
            vector_dimension=VECTOR_DIMENSION # Use the determined dimension
        )
        # Verify the connection after initialization
        if not redis_cache_manager.client or not redis_cache_manager.client.ping():
             logger.warning("Redis connection test failed. Caching layer is disabled.")
             redis_cache_manager = None # Explicitly set to None if connection fails
        else:
            logger.info("Redis Cache Manager initialized and connected successfully.")
    except Exception as e:
         logger.error(f"Failed to initialize Redis Cache Manager: {e}")
         redis_cache_manager = None # Explicitly set to None on any initialization error


    logger.info("--- Resource loading complete ---")

load_resources()

# --- API Endpoint Definition ---
@app.route('/api/agentic_qna', methods=['POST'])
def agentic_qna_endpoint():
    """
    Handles incoming Q&A requests via a POST endpoint.
    Expects JSON with 'query', 'conversation_id', and 'conversation_history'.
    Returns JSON with 'answer', 'confidence_score', 'sources', 'agent_path_taken', etc.
    """
    # --- 4.3 Error Handling: Input Validation ---
    request_data = request.get_json()

    if not request_data:
        logger.error("Received empty or non-JSON request body.")
        return jsonify({"error": "Invalid request: Request body must be JSON."}), 400

    user_query = request_data.get('query')
    conversation_id = request_data.get('conversation_id') # Accepted but currently ignored
    conversation_history = request_data.get('conversation_history') # Accepted but currently ignored

    if not user_query or not isinstance(user_query, str):
        logger.error(f"Invalid or missing 'query' in request: {request_data}")
        return jsonify({"error": "Invalid request: 'query' parameter (string) is required and cannot be empty."}), 400

    logger.info(f"Received query: '{user_query}'")
    # Log conversation_id/history if received (as per requirement to accept)
    if conversation_id is not None: logger.info(f"Received conversation_id: {conversation_id} (currently ignored)")
    if conversation_history is not None: logger.info(f"Received conversation_history (list, currently ignored)")


    # --- 4.3 Error Handling: System Not Ready ---
    # Check if essential FAISS index is loaded
    if faiss_index is None or faiss_metadata is None:
         logger.error("API called but essential FAISS resources were not loaded at startup.")
         # Provide a more informative error message to the user
         return jsonify({
             "answer": "I am currently unable to process your request. The internal knowledge base is not available.",
             "path_taken": "System_Error", # Use path_taken for internal state in response
             "sources": [],
             "confidence_score": 0.0,
             "error": "System not ready: Required data indexes failed to load."
        }), 503 # Service Unavailable - Indicates the service is temporarily down or misconfigured

    # --- 4.3 Error Handling: Internal Processing Error ---
    try:
        # --- 6. Call Core Agentic Logic ---
        # Pass the loaded resources to the agentic function
        response = answer_query_agentic_with_cache(
            user_query,
            faiss_index,
            faiss_metadata,
            bm25_index,        # Pass potentially None BM25 index
            redis_cache_manager # Pass potentially None Redis manager
        )

        # --- 7. Response Formatting (Requirement 4.2) ---
        # The function already returns a dictionary in the desired format,
        # including answer, confidence_score, sources, path_taken.
        # We just map 'path_taken' to 'agent_path_taken' for the API response
        # and ensure all expected fields are present, even if empty/default.
        response_payload = {
            "answer": response.get("answer", "An error occurred while generating the answer."),
            "confidence_score": response.get("confidence_score", 0.0),
            "sources": response.get("sources", []),
            "agent_path_taken": response.get("path_taken", "Processing_Error"), # Map internal name to API name
            "cached": response.get("cached", False), # Add cache status
            "cache_similarity": response.get("cache_similarity") # Add similarity if cached
        }

        logger.info(f"Request processed. Path: {response_payload['agent_path_taken']}, Confidence: {response_payload['confidence_score']:.2f}, Cached: {response_payload['cached']}")

        # --- Return JSON response with success status ---
        return jsonify(response_payload), 200

    except Exception as e:
        # Catch any other unexpected errors during the agent's execution
        logger.exception(f"An unexpected error occurred during agent processing for query: '{user_query}'")
        # Return a generic 500 Internal Server Error response to the user
        return jsonify({
            "answer": "I'm sorry, but I encountered an unexpected error while trying to answer your question. Please try again later.",
            "confidence_score": 0.0,
            "sources": [],
            "agent_path_taken": "Internal_Exception", # Indicate failure path
            "error": "An internal server error occurred."
        }), 500


# --- 9. Running the Flask App ---
if __name__ == '__main__':
    # When running locally, use app.run()
    # In a production environment, use a WSGI server like Gunicorn or uWSGI
    # Use host='0.0.0.0' to make the server accessible from outside the container/localhost
    # debug=True should ONLY be used during development as it exposes detailed errors
    logger.info("Starting Flask development server...")
    app.run(debug=False, host='0.0.0.0', port=8000)


"""
curl -X POST \
  -H "Content-Type: application/json" \
  -d '{"query": "What is the company policy on vacation days?"}' \
  http://127.0.0.1:8000/api/agentic_qna
"""