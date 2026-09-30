import { useEffect, useState } from 'react';
import { booksApi, getApiErrorMessage } from '../api';
import type { Book, PagedResult } from '../types';
import { BookModal } from '../components/BookModal';

export function BooksPage() {
    const [data, setData] = useState<PagedResult<Book> | null>(null);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [modalOpen, setModalOpen] = useState(false);
    const [editingBook, setEditingBook] = useState<Book | null>(null);

    const pageSize = 5;

    async function load() {
        setLoading(true);
        setError(null);
        try {
            setData(await booksApi.list(page, pageSize));
        } catch (e) {
            setError(getApiErrorMessage(e));
        } finally {
            setLoading(false);
        }
    }

   useEffect(() => {
     let cancelled = false;

     async function fetchBooks() {
       try {
         const result = await booksApi.list(page, pageSize);
         if (!cancelled) {
           setData(result);
           setError(null);
         }
       } catch (e) {
         if (!cancelled) {
           setError(getApiErrorMessage(e));
         }
       } finally {
         if (!cancelled) {
           setLoading(false);
         }
       }
     }

     fetchBooks();

     return () => {
       cancelled = true;
     };
   }, [page, pageSize]);

    function openCreate() {
        setEditingBook(null);
        setModalOpen(true);
    }

    function openEdit(book: Book) {
        setEditingBook(book);
        setModalOpen(true);
    }

    async function handleSaved() {
        setModalOpen(false);
        await load();
    }

    async function remove(book: Book) {
        if (!confirm(`Delete "${book.title}"?`)) {
            return;
        }
        try {
            await booksApi.remove(book.id);
            if (data && data.items.length === 1 && page > 1) {
                setPage(page - 1);
            } else {
                await load();
            }
        } catch (e) {
            setError(getApiErrorMessage(e));
        }
    }

    return (
        <div>
            <div className="page-header">
                <h1>Books</h1>
                <button type="button" className="primary" onClick={openCreate}>
                    + New book
                </button>
            </div>

            {error && <p className="error">{error}</p>}
            {loading && <p>Loading…</p>}

            {data && (
                <>
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Author</th>
                                <th>Genre</th>
                                <th className="num">Price</th>
                                <th className="actions">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {data.items.map((b) => (
                                <tr key={b.id}>
                                    <td>{b.title}</td>
                                    <td>{b.author}</td>
                                    <td>
                                        <span className="badge">{b.genre}</span>
                                    </td>
                                    <td className="num">{b.price.toFixed(2)} kr</td>
                                    <td className="actions">
                                        <button type="button" onClick={() => openEdit(b)}>
                                            Edit
                                        </button>
                                        <button
                                            type="button"
                                            className="danger"
                                            onClick={() => remove(b)}
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {data.items.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="empty">
                                        No books yet.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>

                    <div className="pager">
                        <button
                            type="button"
                            disabled={!data.hasPrevious}
                            onClick={() => setPage((p) => p - 1)}
                        >
                            ‹ Prev
                        </button>
                        <span>
                            Page {data.page} of {data.totalPages || 1} · {data.totalCount} total
                        </span>
                        <button
                            type="button"
                            disabled={!data.hasNext}
                            onClick={() => setPage((p) => p + 1)}
                        >
                            Next ›
                        </button>
                    </div>
                </>
            )}

            {modalOpen && (
                <BookModal
                    book={editingBook}
                    onClose={() => setModalOpen(false)}
                    onSaved={handleSaved}
                />
            )}
        </div>
    );
}
