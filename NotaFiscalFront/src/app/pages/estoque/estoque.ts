import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-estoque',
  imports: [FormsModule],
  templateUrl: './estoque.html',
  styleUrl: './estoque.css',
})
export class Estoque implements OnInit {
  produto = {
    codigo: '',
    descricao: '',
    saldo: 0,
  };

  produtos: any[] = [];

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.listar();
  }

  listar() {
    this.http.get<any[]>('http://localhost:5226/api/Estoque').subscribe({
      next: (data) => {
        this.produtos = [...data];
        this.cdr.detectChanges();
      },
      error: (err) => {
        alert('Erro ao listar produtos: ' + err.message);
      },
    });
  }

  cadastrar() {
    this.http.post('http://localhost:5226/api/Estoque', this.produto).subscribe({
      next: () => {
        alert('Produto cadastrado com sucesso!');
        this.produto = { codigo: '', descricao: '', saldo: 0 };
        this.listar();
      },
      error: (err) => {
        alert('Erro ao cadastrar produto: ' + err.message);
      },
    });
  }
}
