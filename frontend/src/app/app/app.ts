import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';

// Shell da aplicação (camada app do FSD, ADR 0009): cabeçalho, menu lateral e área das páginas
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, NzLayoutModule, NzMenuModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly menu: { rota: string; titulo: string }[] = [];
}
