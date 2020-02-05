#!/usr/bin/env node
const {status} = require('child_process').spawnSync(
	require('./'),
	process.argv.slice(2),
	{ shell: true, stdio: 'inherit' }
)
process.exit(status)
