const sql = require('mssql');

const config = {
    user: 'sa',
    password: 'Admin@gd4',
    server: '10.35.222.66', 
    database: 'SEMed1650',
    port: 1433,
    options: {
        encrypt: false, // For older SQL Server
        trustServerCertificate: true 
    }
};

async function explore() {
    try {
        await sql.connect(config);
        const result = await sql.query(`
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_type = 'BASE TABLE'
        `);
        console.log('Tables in SEMed1650:');
        console.log(result.recordset.map(r => r.table_name).join(', '));
    } catch (err) {
        console.error('SQL error', err);
    } finally {
        process.exit(0);
    }
}

explore();
