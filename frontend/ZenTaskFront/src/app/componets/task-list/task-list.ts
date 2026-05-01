import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaskService } from '../../services/task-service';
import { Task, CreateTaskDto } from '../../interfaces/task';


export type FilterValue = 'all' | 'pending' | 'completed';
 
export interface Filter {
  label: string;
  value: FilterValue;
}

@Component({
  selector: 'app-task-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './task-list.html',
  styleUrl: './task-list.css',
})


export class TaskList implements OnInit {
  // ── Estado ────────────────────────────────────────────────────────────────
 
  tasks: Task[] = [];
  activeFilter: FilterValue = 'all';
  showModal = false;
  isLoading = false;
  errorMsg = '';
 
  filters: Filter[] = [
    { label: 'Todas',       value: 'all'       },
    { label: 'Pendientes',  value: 'pending'   },
    { label: 'Completadas', value: 'completed' },
  ];
 
  newTask: CreateTaskDto = {
    title:     '',
    due:       'Hoy',
    time:      '09:00',
    category:  'Trabajo',
    completed: false
  };
 
  // ── Constructor ───────────────────────────────────────────────────────────
 
  constructor(private taskService: TaskService) {}
 
  // ── Lifecycle ─────────────────────────────────────────────────────────────
 
  ngOnInit(): void {
    this.loadTasks();
  }
 
  // ── Carga de tareas ───────────────────────────────────────────────────────
 
  loadTasks(): void {
    this.isLoading = true;
    this.errorMsg  = '';
 
    this.taskService.getAll().subscribe({
      next: (data) => {
        this.tasks     = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMsg  = 'No se pudieron cargar las tareas. Verifica que el servidor esté corriendo.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }
 
  // ── Computed ──────────────────────────────────────────────────────────────
 
  get filteredTasks(): Task[] {
    switch (this.activeFilter) {
      case 'pending':   return this.tasks.filter(t => !t.completed);
      case 'completed': return this.tasks.filter(t =>  t.completed);
      default:          return this.tasks;
    }
  }
 
  // ── Filtros ───────────────────────────────────────────────────────────────
 
  setFilter(filter: FilterValue): void {
    this.activeFilter = filter;
  }
 
  // ── Toggle completada ─────────────────────────────────────────────────────
 
  toggleTask(id: number): void {
    const task = this.tasks.find(t => t.id === id);
    if (!task) return;
 
    // Optimistic update — actualiza la UI antes de esperar la API
    task.completed = !task.completed;
 
    this.taskService.toggleCompleted({ ...task, completed: !task.completed }).subscribe({
      error: (err) => {
        // Revertir si la API falla
        task.completed = !task.completed;
        console.error('Error al actualizar tarea:', err);
      }
    });
  }
 
  // ── Agregar tarea ─────────────────────────────────────────────────────────
 
  openAddModal(): void {
    this.newTask = { title: '', due: 'Hoy', time: '09:00', category: 'Trabajo', completed: false };
    this.showModal = true;
  }
 
  closeModal(): void {
    this.showModal = false;
  }
 
  addTask(): void {
    if (!this.newTask.title.trim()) return;
 
    this.taskService.create(this.newTask).subscribe({
      next: (created) => {
        this.tasks.push(created);
        this.closeModal();
      },
      error: (err) => {
        console.error('Error al crear tarea:', err);
      }
    });
  }
 
  // ── Eliminar tarea ────────────────────────────────────────────────────────
 
  deleteTask(id: number): void {
    this.taskService.delete(id).subscribe({
      next: () => {
        this.tasks = this.tasks.filter(t => t.id !== id);
      },
      error: (err) => {
        console.error('Error al eliminar tarea:', err);
      }
    });
  }
  
}
