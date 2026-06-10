import axios from 'axios';
import type {
    Book,
    BookInput,
    Order,
    OrderInput,
    PagedResult,
    ValidationProblem,
} from './types';

const http = axios.create({
    baseURL: '/api',
    headers: { 'Content-Type': 'application/json' },
});

export function getApiErrorMessage(error: unknown): string {
    if (axios.isAxiosError(error)) {
        const data = error.response?.data as ValidationProblem | string | undefined;
        if (typeof data === 'string') {
            return data;
        }
        if (data?.errors) {
            return Object.values(data.errors).flat().join('\n');
        }
        if (data?.title) {
            return data.title;
        }
        return error.message;
    }
    return 'An unexpected error occurred.';
}

// --- Books ---

export const booksApi = {
    list: (page = 1, pageSize = 10) =>
        http
            .get<PagedResult<Book>>('/books', { params: { page, pageSize } })
            .then((r) => r.data),

    get: (id: number) => http.get<Book>(`/books/${id}`).then((r) => r.data),

    create: (input: BookInput) =>
        http.post<Book>('/books', input).then((r) => r.data),

    update: (id: number, input: BookInput) =>
        http.put<Book>(`/books/${id}`, { id, ...input }).then((r) => r.data),

    remove: (id: number) => http.delete(`/books/${id}`).then(() => undefined),
};

// --- Orders ---

export const ordersApi = {
    list: (page = 1, pageSize = 10) =>
        http
            .get<PagedResult<Order>>('/orders', { params: { page, pageSize } })
            .then((r) => r.data),

    get: (id: number) => http.get<Order>(`/orders/${id}`).then((r) => r.data),

    create: (input: OrderInput) =>
        http.post<Order>('/orders', input).then((r) => r.data),

    update: (id: number, input: OrderInput) =>
        http.put<Order>(`/orders/${id}`, { id, ...input }).then((r) => r.data),

    remove: (id: number) => http.delete(`/orders/${id}`).then(() => undefined),

    addBook: (orderId: number, bookId: number, quantity = 1) =>
        http
            .post<Order>(`/orders/${orderId}/books`, null, {
                params: { bookId, quantity },
            })
            .then((r) => r.data),
};
