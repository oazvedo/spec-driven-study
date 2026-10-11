import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NzTagModule } from 'ng-zorro-antd/tag';

@Component({
  selector: 'app-usuario-status-tag',
  imports: [NzTagModule],
  template: `<nz-tag [nzColor]="ativo() ? 'success' : 'default'">{{ ativo() ? 'Ativo' : 'Inativo' }}</nz-tag>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioStatusTag {
  readonly ativo = input.required<boolean>();
}
