const express = require('express');
const app = express();

app.get('/api/hello', (req,res) => {
    res.json({message: 'Hello from the docker backend'});
});

app.listen(3000, () => {
    console.log('Backend is running on port 3000');
});