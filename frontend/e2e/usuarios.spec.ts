import { expect, test } from '@playwright/test';

test.beforeEach(async ({ page }) => {
  await page.goto('/usuarios');
});

// CA-03
test('cadastrar usuário e vê-lo na tabela', async ({ page }) => {
  const email = `e2e-${Date.now()}@exemplo.com`;

  await page.getByRole('button', { name: 'Novo usuário' }).click();
  await page.getByLabel('Nome').fill('Usuária E2E');
  await page.getByLabel('E-mail').fill(email);
  await page.getByRole('button', { name: 'Salvar' }).click();

  await expect(page.getByText('Usuário cadastrado.')).toBeVisible();
  // RN-05: os mais recentes aparecem primeiro, então o novo usuário está na primeira página
  const linha = page.getByRole('row').filter({ hasText: email });
  await expect(linha).toContainText('Usuária E2E');
  await expect(linha).toContainText('Ativo');
});

// CA-04
test('submeter formulário vazio mostra erros e não cria', async ({ page }) => {
  const posts: string[] = [];
  page.on('request', (req) => {
    if (req.method() === 'POST' && req.url().includes('/api/usuarios')) posts.push(req.url());
  });

  await page.getByRole('button', { name: 'Novo usuário' }).click();
  await page.getByRole('button', { name: 'Salvar' }).click();

  await expect(page.getByText('O nome é obrigatório.')).toBeVisible();
  await expect(page.getByText('O e-mail é obrigatório.')).toBeVisible();
  expect(posts).toEqual([]);
});
