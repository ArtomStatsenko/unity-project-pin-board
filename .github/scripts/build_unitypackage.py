import io
import os
import re
import tarfile

_ROOT_FILES = (
    'package.json',
    'README.md',
    'README_ZH.md',
    'CHANGELOG.md',
    'LICENSE.md',
)

_DEFAULT_ASSETS_PREFIX = 'Assets/Project Pin Board'


def _repo_root():
    return os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))


def get_guid(meta_path):
    with open(meta_path, encoding='utf-8', errors='replace') as f:
        for line in f:
            m = re.search(r'guid:\s*(\w+)', line)
            if m:
                return m.group(1)
    return None


def _assets_prefix():
    raw = os.environ.get('UNITYPACKAGE_ASSETS_PREFIX', _DEFAULT_ASSETS_PREFIX)
    return raw.strip().replace('\\', '/').strip('/')


def _pathname_bytes(path, base, assets_prefix):
    r = os.path.relpath(path, base).replace(os.sep, '/')
    if assets_prefix:
        r = f'{assets_prefix}/{r}'
    return r.encode('utf-8')


def _add_pathname(tar, guid, data):
    info = tarfile.TarInfo(name=f'{guid}/pathname')
    info.size = len(data)
    tar.addfile(info, io.BytesIO(data))


def add_folder(tar, dir_path, base, assets_prefix):
    meta_path = dir_path + '.meta'
    if not os.path.isfile(meta_path):
        return
    guid = get_guid(meta_path)
    if not guid:
        return
    _add_pathname(tar, guid, _pathname_bytes(dir_path, base, assets_prefix))
    tar.add(meta_path, arcname=f'{guid}/asset.meta')


def add_file(tar, fpath, base, assets_prefix):
    mpath = fpath + '.meta'
    if not os.path.isfile(mpath):
        return
    guid = get_guid(mpath)
    if not guid:
        return
    tar.add(fpath, arcname=f'{guid}/asset')
    tar.add(mpath, arcname=f'{guid}/asset.meta')
    _add_pathname(tar, guid, _pathname_bytes(fpath, base, assets_prefix))


def build_editor_tree(tar, base, assets_prefix):
    editor_root = os.path.join(base, 'Editor')
    if not os.path.isdir(editor_root):
        raise SystemExit(f'Missing Editor folder: {editor_root}')
    for root, _dirs, files in os.walk(editor_root):
        add_folder(tar, root, base, assets_prefix)
        for fname in files:
            if fname.endswith('.meta'):
                continue
            fpath = os.path.join(root, fname)
            add_file(tar, fpath, base, assets_prefix)


def build_root_allowlist(tar, base, assets_prefix):
    for name in _ROOT_FILES:
        fpath = os.path.join(base, name)
        if os.path.isfile(fpath) and os.path.isfile(fpath + '.meta'):
            add_file(tar, fpath, base, assets_prefix)


def main():
    env = os.environ.get('UNITY_PACKAGE_REPO_ROOT')
    base = os.path.abspath(env) if env else _repo_root()
    assets_prefix = _assets_prefix()
    out = os.path.join(base, 'ProjectPinBoard.unitypackage')
    with tarfile.open(out, 'w:gz') as tar:
        build_editor_tree(tar, base, assets_prefix)
        build_root_allowlist(tar, base, assets_prefix)
    print(f'Wrote {out}')


if __name__ == '__main__':
    main()
