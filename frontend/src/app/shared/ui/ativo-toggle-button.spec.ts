import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzPopconfirmDirective } from 'ng-zorro-antd/popconfirm';
import { AtivoToggleButton } from './ativo-toggle-button';

describe('AtivoToggleButton', () => {
  let fixture: ComponentFixture<AtivoToggleButton>;
  let confirmacoes: number;

  function renderizar(ativo: boolean): void {
    fixture = TestBed.createComponent(AtivoToggleButton);
    fixture.componentRef.setInput('ativo', ativo);
    fixture.componentRef.setInput('rotulo', 'Ana');
    fixture.componentRef.setInput('confirmacao', 'Inativar este usuário?');
    confirmacoes = 0;
    fixture.componentInstance.confirmar.subscribe(() => confirmacoes++);
    fixture.detectChanges();
  }

  const botao = () => fixture.nativeElement.querySelector('button') as HTMLButtonElement;
  const popconfirm = () => fixture.debugElement.query(By.directive(NzPopconfirmDirective)).injector.get(NzPopconfirmDirective);

  it('ativo: mostra "Inativar" e só confirma depois do popconfirm', () => {
    renderizar(true);

    expect(botao().textContent?.trim()).toBe('Inativar');
    expect(botao().getAttribute('aria-label')).toBe('Inativar Ana');
    botao().click();
    expect(confirmacoes).toBe(0);

    popconfirm().nzOnConfirm.emit();

    expect(confirmacoes).toBe(1);
  });

  it('inativo: mostra "Ativar" e confirma direto no clique', () => {
    renderizar(false);

    expect(botao().textContent?.trim()).toBe('Ativar');
    botao().click();

    expect(confirmacoes).toBe(1);
  });

  // Falha: cancelar o popconfirm não confirma
  it('ativo: cancelar o popconfirm não confirma', () => {
    renderizar(true);

    botao().click();
    popconfirm().nzOnCancel.emit();

    expect(confirmacoes).toBe(0);
  });
});
