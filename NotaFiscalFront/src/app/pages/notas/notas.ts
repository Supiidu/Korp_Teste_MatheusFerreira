import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-notas',
  imports: [FormsModule, CommonModule],
  templateUrl: './notas.html',
  styleUrl: './notas.css',
})
export class Notas implements OnInit {
  tabAtiva = 'criar';

  notas: any[] = [];
  notaSelecionada: any = null;
  produtos: any[] = [];

  novoItem = {
    codigoProduto: '',
    quantidade: 0,
  };

  erroBaixar = '';
  fechando = false;

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.listarNotas();
    this.listarProdutos();
  }

  listarNotas() {
    this.http.get<any[]>('http://localhost:5072/api/NotaFiscal').subscribe({
      next: (data) => {
        this.notas = [...data];
        this.cdr.detectChanges();
      },
      error: (err) => alert('Erro ao listar notas: ' + err.message),
    });
  }
  listarProdutos() {
    this.http.get<any[]>('http://localhost:5226/api/Estoque').subscribe({
      next: (data) => {
        this.produtos = [...data];
        this.cdr.detectChanges();
      },
      error: (err) => alert('Erro ao carregar produtos: ' + err.message),
    });
  }

  criarNota() {
    this.http.post('http://localhost:5072/api/NotaFiscal', {}).subscribe({
      next: (nota: any) => {
        alert(`Nota #${nota.numero} criada com sucesso!`);
        this.listarNotas();
        this.tabAtiva = 'visualizar';
      },
      error: (err) => alert('Erro ao criar nota: ' + err.message),
    });
  }

  selecionarNota(nota: any) {
    this.http.get<any>(`http://localhost:5072/api/NotaFiscal/${nota.id}`).subscribe({
      next: (data) => {
        this.notaSelecionada = null; // 👈 reseta primeiro
        setTimeout(() => {
          this.notaSelecionada = data;
          this.cdr.detectChanges();
        }, 0);
      },
      error: (err) => alert('Erro ao carregar nota: ' + err.message),
    });
  }

  adicionarItem() {
    this.http
      .post(`http://localhost:5072/api/NotaFiscal/${this.notaSelecionada.id}/itens`, this.novoItem)
      .subscribe({
        next: () => {
          this.novoItem = { codigoProduto: '', quantidade: 0 };
          this.selecionarNota(this.notaSelecionada);
        },
        error: (err) => alert('Erro ao adicionar item: ' + err.message),
      });
  }

  fecharNota() {
    this.fechando = true;
    this.erroBaixar = '';
    this.http
      .post(
        `http://localhost:5072/api/NotaFiscal/${this.notaSelecionada.id}/fechar`,
        {},
        { responseType: 'text' },
      )
      .pipe(
        finalize(() => {
          this.fechando = false;
          this.cdr.detectChanges();
        }),
      )
      .subscribe({
        next: () => {
          this.selecionarNota(this.notaSelecionada);
          this.listarNotas();
        },
        error: (err) => {
          this.erroBaixar = typeof err.error === 'string' ? err.error : 'Erro ao fechar nota.';
          this.selecionarNota(this.notaSelecionada);
          this.listarNotas();
        },
      });
  }
  removerItem(itemId: number) {
    this.http
      .delete(`http://localhost:5072/api/NotaFiscal/${this.notaSelecionada.id}/itens/${itemId}`)
      .subscribe({
        next: () => {
          this.selecionarNota(this.notaSelecionada);

          console.log('removendo item:', itemId);
        },
        error: (err) => alert('Erro ao remover item: ' + err.message),
      });
  }
}
