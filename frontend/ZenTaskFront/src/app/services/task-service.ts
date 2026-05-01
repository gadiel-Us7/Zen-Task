import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environments';
import { Task, CreateTaskDto, UpdateTaskDto } from '../interfaces/task';

@Injectable({
  providedIn: 'root'
})
export class TaskService {

  private readonly url = `${environment.apiUrl}/tareas`;

  constructor(private http: HttpClient) {}

  // GET /api/tareas — trae todas, o filtra por completada
  getAll(completada?: boolean): Observable<Task[]> {
    let params = new HttpParams();
    if (completada !== undefined) {
      params = params.set('completada', completada.toString());
    }
    return this.http.get<Task[]>(this.url, { params });
  }

  // GET /api/tareas/:id — trae una sola tarea
  getById(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.url}/${id}`);
  }

  // POST /api/tareas — crea una tarea nueva (sin id)
  create(task: CreateTaskDto): Observable<Task> {
    return this.http.post<Task>(this.url, task);
  }

  // PUT /api/tareas/:id — actualiza todos los campos
  update(id: number, task: UpdateTaskDto): Observable<Task> {
    return this.http.put<Task>(`${this.url}/${id}`, task);
  }

  // DELETE /api/tareas/:id — elimina una tarea
  delete(id: number): Observable<{ mensaje: string }> {
    return this.http.delete<{ mensaje: string }>(`${this.url}/${id}`);
  }

  // Atajo para solo cambiar el estado completado
  toggleCompleted(task: Task): Observable<Task> {
    return this.update(task.id, {
      title:     task.title,
      due:       task.due,
      time:      task.time,
      category:  task.category,
      completed: !task.completed
    });
  }
}