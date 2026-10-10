import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App (shell)', () => {
  beforeEach(() => TestBed.configureTestingModule({ imports: [App], providers: [provideRouter([])] }));

  it('renderiza o cabeçalho, o menu lateral e a área das páginas', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const html = fixture.nativeElement as HTMLElement;

    expect(html.querySelector('.titulo')?.textContent).toContain('Template CRUD');
    expect(html.querySelector('ul[nz-menu]')).not.toBeNull();
    expect(html.querySelector('router-outlet')).not.toBeNull();
  });
});
