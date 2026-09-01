const axios = require('axios');

async function testApi() {
    try {
        const drugCodes = ['1180440', '1290030^100', '1130100^100']; 
        const apiUrl = 'http://192.168.34.246/apiopd/dih/getsemedstock';
        console.log('Fetching', apiUrl, 'with', drugCodes);
        const res = await axios.post(apiUrl, { drugcode: drugCodes }, { timeout: 10000 });
        console.log('Response:', JSON.stringify(res.data, null, 2));
    } catch (err) {
        console.error('Error:', err.message);
        if (err.response) console.error(err.response.data);
    }
}

testApi();
