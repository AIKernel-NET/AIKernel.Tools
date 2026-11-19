"""Run real CLI subprocesses against test-only native ABI fixtures; never touch a GPU."""
from pathlib import Path
import subprocess
import sys

cli, root = Path(sys.argv[1]).resolve(), Path(sys.argv[2]).resolve()
cases = dict(good='cuda-driver-host-softmax: ok', wrong_version='cuda-driver-abi: unsupported',
             missing_export='export: ak_attention_tile missing', wrong_result='invalid-result',
             nonfinite_result='invalid-result',
             accepts_invalid='cuda-driver-invalid-arguments: not-rejected')
for name, expected in cases.items():
    candidates = list(root.rglob(name+'.dll')) if sys.platform == 'win32' else list(root.rglob('lib'+name+'.so'))
    assert len(candidates) == 1, (name, candidates)
    result = subprocess.run(['dotnet',str(cli),'gpu','verify-native','--provider','cuda-driver','--library',str(candidates[0])],
                            text=True,capture_output=True,timeout=60)
    assert result.returncode == (0 if name == 'good' else 1), result.stdout+result.stderr
    assert expected in result.stdout, result.stdout
    assert 'AIKERNEL_LIBTORCH_PATH' not in result.stdout
    print(f'{name}: expected result verified')
