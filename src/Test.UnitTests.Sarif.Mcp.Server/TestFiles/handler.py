def execute_job(request):
    command = request.json['command']
    timeout = request.json.get('timeout', 30)
    result = subprocess.run(command, shell=True, capture_output=True, timeout=timeout)
    return {'stdout': result.stdout.decode(), 'stderr': result.stderr.decode()}

def health():
    return {'status': 'ok'}
