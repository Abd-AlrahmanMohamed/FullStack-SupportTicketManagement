import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TicketService } from './ticket.service';
import { environment } from '../../../environments/environment';
import { PaginatedResult, Ticket } from '../models/ticket.model';

describe('TicketService', () => {
  let service: TicketService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TicketService]
    });

    service = TestBed.inject(TicketService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('builds the correct query string for pagination, filters, search, and sort', () => {
    const emptyResult: PaginatedResult<Ticket> = {
      items: [],
      page: 2,
      pageSize: 25,
      totalCount: 0,
      totalPages: 0
    };

    service
      .getTickets({
        page: 2,
        pageSize: 25,
        status: 'Open',
        priority: 'Critical',
        search: 'login',
        sortBy: 'priority',
        sortDirection: 'desc'
      })
      .subscribe((result) => expect(result).toEqual(emptyResult));

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/tickets` && r.method === 'GET'
    );

    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('25');
    expect(req.request.params.get('status')).toBe('Open');
    expect(req.request.params.get('priority')).toBe('Critical');
    expect(req.request.params.get('search')).toBe('login');
    expect(req.request.params.get('sortBy')).toBe('priority');
    expect(req.request.params.get('sortDirection')).toBe('desc');

    req.flush(emptyResult);
  });

  it('omits optional filters entirely when not provided', () => {
    service.getTickets({ page: 1, pageSize: 10 }).subscribe();

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/tickets` && r.method === 'GET'
    );

    expect(req.request.params.has('status')).toBeFalse();
    expect(req.request.params.has('search')).toBeFalse();

    req.flush({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  });

  it('posts a new ticket without a client-supplied customerId field affecting the request body shape', () => {
    service.createTicket({ title: 'Cannot log in', description: 'Details', priority: 'High' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/tickets`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      title: 'Cannot log in',
      description: 'Details',
      priority: 'High'
    });

    req.flush({});
  });
});
