// Teste de arquitetura do frontend (ADR 0009 e ADR 0010): garante que as regras do FSD no ESLint
// continuam barrando imports proibidos. Sem ele, um padrão errado ou um upgrade do
// eslint-plugin-boundaries deixaria o `npm run lint` verde sem checar nada.
// Os arquivos de exemplo ficam numa pasta temporária, fora de src/.
import { test, before, after } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { ESLint } from 'eslint';

const arquivos = {
  'src/app/entities/x/model/a.ts': 'export const a = 1;\n',
  'src/app/entities/x/index.ts': "export { a } from './model/a';\n",
  'src/app/entities/x/importa-feature.ts': "import { y } from '../../features/y';\nexport const r = y;\n",
  'src/app/features/y/index.ts': "import { a } from '../../entities/x';\nexport const y = a;\n",
  'src/app/features/y/fura-api-publica.ts': "import { a } from '../../entities/x/model/a';\nexport const f = a;\n",
  'src/app/features/w/index.ts': "import { y } from '../y';\nexport const w = y;\n",
};

let raiz;
let eslint;

before(() => {
  raiz = mkdtempSync(join(tmpdir(), 'fsd-lint-'));
  for (const [caminho, conteudo] of Object.entries(arquivos)) {
    mkdirSync(dirname(join(raiz, caminho)), { recursive: true });
    writeFileSync(join(raiz, caminho), conteudo);
  }
  eslint = new ESLint({ cwd: raiz, overrideConfigFile: fileURLToPath(new URL('./eslint.config.js', import.meta.url)) });
});

after(() => rmSync(raiz, { recursive: true, force: true }));

async function errosDeFronteira(caminho) {
  const [resultado] = await eslint.lintFiles([join(raiz, caminho)]);
  return resultado.messages.filter((m) => m.ruleId === 'boundaries/dependencies');
}

for (const caminho of ['src/app/entities/x/index.ts', 'src/app/features/y/index.ts']) {
  test(`permitido: ${caminho}`, async () => {
    assert.deepEqual(await errosDeFronteira(caminho), []);
  });
}

for (const [caminho, motivo] of [
  ['src/app/entities/x/importa-feature.ts', 'camada de baixo importando camada de cima'],
  ['src/app/features/w/index.ts', 'slice importando slice irmã da mesma camada'],
  ['src/app/features/y/fura-api-publica.ts', 'import que fura a API pública (index.ts)'],
]) {
  test(`bloqueado (${motivo}): ${caminho}`, async () => {
    assert.equal((await errosDeFronteira(caminho)).length, 1);
  });
}
