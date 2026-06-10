// Shared API types mirroring the BookStore.Api contracts.

export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
}

export interface Book {
    id: number;
    title: string;
    author: string;
    genre: string;
    description: string;
    price: number;
}

export type BookInput = Omit<Book, 'id'>;

export interface OrderItem {
    bookId: number;
    title: string;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
}

export interface Order {
    id: number;
    userEmail: string;
    totalPrice: number;
    items: OrderItem[];
}

export interface OrderItemInput {
    bookId: number;
    quantity: number;
}

export interface OrderInput {
    userEmail: string;
    items: OrderItemInput[];
}

/** Shape of an ASP.NET ValidationProblemDetails response. */
export interface ValidationProblem {
    title?: string;
    status?: number;
    errors?: Record<string, string[]>;
}
