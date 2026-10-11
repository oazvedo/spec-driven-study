import { TestBed } from '@angular/core/testing';
import { UsuarioStatusTag } from './usuario-status-tag';

describe('UsuarioStatusTag', () => {
  function renderizar(ativo: boolean): HTMLElement {
    const fixture = TestBed.createComponent(UsuarioStatusTag);
    fixture.componentRef.setInput('ativo', ativo);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('mostra "Ativo" para usuário ativo', () => {
    expect(renderizar(true).textContent?.trim()).toBe('Ativo');
  });

  it('mostra "Inativo" para usuário inativo', () => {
    expect(renderizar(false).textContent?.trim()).toBe('Inativo');
  });
});
