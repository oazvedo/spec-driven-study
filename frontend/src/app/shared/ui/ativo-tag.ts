import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NzTagModule } from 'ng-zorro-antd/tag';

// Status Ativo/Inativo da exclusão lógica (ADR 0008), igual para toda entidade
@Component({
  selector: 'app-ativo-tag',
  imports: [NzTagModule],
  template: `<nz-tag [nzColor]="ativo() ? 'success' : 'default'">{{ ativo() ? 'Ativo' : 'Inativo' }}</nz-tag>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AtivoTag {
  readonly ativo = input.required<boolean>();
}
