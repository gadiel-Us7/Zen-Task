export interface Task {
  id: number;
  title: string;
  due: string;
  time: string;
  category: string;
  completed: boolean;
}

// Lo que se envía al crear — sin id (lo genera SQL Server)
export interface CreateTaskDto {
  title:     string;
  due:       string;
  time:      string;
  category:  string;
  completed: boolean;
}

// Lo que se envía al actualizar — todos los campos editables
export interface UpdateTaskDto {
  title:     string;
  due:       string;
  time:      string;
  category:  string;
  completed: boolean;
}
 