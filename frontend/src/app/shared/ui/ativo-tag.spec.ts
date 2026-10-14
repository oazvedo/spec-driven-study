import { TestBed } from '@angular/core/testing';
import { AtivoTag } from './ativo-tag';

describe('AtivoTag', () => {
  function renderizar(ativo: boolean): HTMLElement {
    const fixture = TestBed.createComponent(AtivoTag);
    fixture.componentRef.setInput('ativo', ativo);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('mostra "Ativo" quando ativo', () => {
    expect(renderizar(true).textContent?.trim()).toBe('Ativo');
  });

  it('mostra "Inativo" quando inativo', () => {
    expect(renderizar(false).textContent?.trim()).toBe('Inativo');
  });
});
