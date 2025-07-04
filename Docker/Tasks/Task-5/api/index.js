const express = require('express');
const mongoose = require('mongoose');

const app = express();
const mongoUrl = 'mongodb://localhost:27017/db';

mongoose.connect(mongoUrl)
    .then(() => console.log('Connected to MongoDB'))
    .catch((err) => console.error('MongoDB connection error', err));

app.get('/', (req,res) => {
    res.send('Node.js is running with MongoDB');
});

app.listen(3000, () => {
    console.log('Node.js is running on port 3000');
});