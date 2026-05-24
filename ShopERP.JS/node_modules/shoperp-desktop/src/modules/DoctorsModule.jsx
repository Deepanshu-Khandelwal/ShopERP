import { useEffect, useMemo, useState } from 'react';
import { listDoctors } from '../services/api';

export default function DoctorsModule() {
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(false);
  const [query, setQuery] = useState('');

  const loadDoctors = async () => {
    setLoading(true);
    try {
      const rows = await listDoctors();
      setDoctors(rows);
    } catch {
      setDoctors([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDoctors();
  }, []);

  const filteredDoctors = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) return doctors;
    return doctors.filter((doctor) => doctor.name.toLowerCase().includes(term));
  }, [doctors, query]);

  return (
    <section className="module-page doctors-page">
      <h3>Doctors</h3>
      <p className="small-row">This list is built from saved billing records.</p>

      <div className="row-2 doctors-filter-row">
        <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search doctor name" />
        <div className="doctor-summary-card">
          <strong>{doctors.length}</strong>
          <span>Doctors found</span>
        </div>
      </div>

      <div className="table-card doctors-table-card">
        <div className="table-card-head">
          <h4>Doctor List</h4>
          <div className="action-group">
            <p>{loading ? 'Loading...' : `${filteredDoctors.length} records`}</p>
            <button type="button" className="secondary-btn" onClick={loadDoctors}>
              Refresh
            </button>
          </div>
        </div>

        <table className="table doctors-table">
          <thead>
            <tr>
              <th>Doctor Name</th>
              <th>Visits</th>
              <th>Last Visit</th>
            </tr>
          </thead>
          <tbody>
            {filteredDoctors.map((doctor) => (
              <tr key={doctor.name}>
                <td>{doctor.name}</td>
                <td>{doctor.visitCount}</td>
                <td>{doctor.lastVisitAt ? new Date(doctor.lastVisitAt).toLocaleDateString('en-GB') : '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>

        {!loading && filteredDoctors.length === 0 && <p className="small-row">No doctors found.</p>}
      </div>
    </section>
  );
}