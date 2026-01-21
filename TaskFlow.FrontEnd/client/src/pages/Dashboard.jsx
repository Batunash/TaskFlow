import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FolderKanban, CheckSquare, Users, Activity, Building2, Plus, ArrowRight, LogOut } from "lucide-react";
import userService from "../services/userService";
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import organizationService from "../services/organizationService";

export default function Dashboard() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState(null);
  const [stats, setStats] = useState({
    projectCount: 0,
    taskCount: 0,
    userRole: ""
  });
  const [orgForm, setOrgForm] = useState({ name: "", description: "" });
  const [creatingOrg, setCreatingOrg] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      const userData = await userService.getMe();
      setUser(userData);
      
      // Check if user has an organization
      if (userData.organizationId) {
        const [projectsData, tasksData] = await Promise.all([
          projectService.getAll(),
          taskService.getAll()
        ]);

        setStats({
          projectCount: Array.isArray(projectsData) ? projectsData.length : 0,
          taskCount: Array.isArray(tasksData) ? tasksData.length : 0,
          userRole: userData.role || "Member"
        });
      }
    } catch (error) {
      console.error("Error fetching dashboard data:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateOrganization = async (e) => {
    e.preventDefault();
    setCreatingOrg(true);
    
    try {
      const response = await organizationService.create(orgForm);
      console.log("Organization created:", response);
      
      // Backend handles casing differently sometimes, checking both
      const token = response.accessToken || response.AccessToken;

      if (token) {
        // 1. Save new token
        localStorage.setItem("token", token);

        // 2. Update user info in local storage
        const currentUser = JSON.parse(localStorage.getItem("user") || "{}");
        const updatedUser = {
            ...currentUser,
            organizationId: response.organization?.id || response.Organization?.id
        };
        localStorage.setItem("user", JSON.stringify(updatedUser));

        alert("Workspace created successfully! Refreshing session...");
        setOrgForm({ name: "", description: "" });
        
        // 3. Force reload to apply new token to axios client
        window.location.reload(); 
        
      } else {
        alert("Organization created but token was missing. Please login again.");
        navigate("/login");
      }

    } catch (error) {
      console.error(error);
      alert(error.response?.data?.message || "Failed to create organization.");
    } finally {
      setCreatingOrg(false);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    navigate("/login");
  };

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center bg-[#111827] text-gray-400">
        <Activity className="animate-spin mr-2" /> Loading...
      </div>
    );
  }

  // State: User has NO Organization
  if (!user?.organizationId) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center p-4 text-white">
        <div className="max-w-2xl w-full bg-[#1F2937] border border-gray-700 rounded-2xl shadow-2xl p-8 md:p-12 relative overflow-hidden">
          <div className="absolute top-0 right-0 w-64 h-64 bg-blue-600/10 rounded-full blur-[80px] -mr-16 -mt-16 pointer-events-none"/>
          <div className="relative z-10">
            <div className="w-16 h-16 bg-blue-900/30 rounded-2xl flex items-center justify-center mb-6 border border-blue-500/20">
              <Building2 className="w-8 h-8 text-blue-400" />
            </div>
            <h1 className="text-3xl font-bold mb-3">Welcome, {user?.userName || "User"}!</h1>
            <p className="text-gray-400 mb-8 text-lg">
              You are not a member of any organization yet. Create a workspace to start managing projects.
            </p>
            <div className="grid md:grid-cols-2 gap-8">
              <div className="space-y-6">
                <h3 className="text-xl font-semibold text-white flex items-center gap-2">
                  <Plus className="w-5 h-5 text-blue-500" />
                  Create Workspace
                </h3>                
                <form onSubmit={handleCreateOrganization} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-400 mb-1">Organization Name</label>
                    <input 
                      type="text" 
                      required
                      className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                      placeholder="e.g. Acme Corp"
                      value={orgForm.name}
                      onChange={(e) => setOrgForm({...orgForm, name: e.target.value})}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-400 mb-1">Description (Optional)</label>
                    <textarea 
                      className="w-full bg-[#111827] border border-gray-600 rounded-lg px-4 py-2.5 text-white focus:ring-2 focus:ring-blue-500 outline-none transition-all h-24 resize-none"
                      placeholder="What is this team about?"
                      value={orgForm.description}
                      onChange={(e) => setOrgForm({...orgForm, description: e.target.value})}
                    />
                  </div>
                  <button 
                    type="submit"
                    disabled={creatingOrg}
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2.5 px-4 rounded-lg transition-colors flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {creatingOrg ? "Creating..." : "Create Organization"}
                    {!creatingOrg && <ArrowRight size={18} />}
                  </button>
                </form>
              </div>
              <div className="border-l border-gray-700 pl-8 flex flex-col justify-center space-y-4 opacity-70 hover:opacity-100 transition-opacity">
                <div className="w-12 h-12 bg-purple-900/30 rounded-xl flex items-center justify-center border border-purple-500/20">
                  <Users className="w-6 h-6 text-purple-400" />
                </div>
                <h3 className="text-xl font-semibold text-white">Joining a team?</h3>
                <p className="text-gray-400 text-sm leading-relaxed">
                  Ask your administrator to send you an invitation email. Once invited, the organization will appear here automatically.
                </p>
                <div className="pt-4 border-t border-gray-700">
                  <button onClick={handleLogout} className="text-gray-500 hover:text-white text-sm flex items-center gap-2 transition-colors">
                    <LogOut size={14} /> Logout
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // State: User HAS Organization
  return (
    <div className="text-white">
      <header className="mb-8 border-b border-gray-800 pb-6">
        <h2 className="text-3xl font-bold text-gray-100">Dashboard</h2>
        <p className="text-gray-400 mt-2 flex items-center gap-2">
          Welcome back, <span className="text-blue-400 font-medium capitalize">{user?.userName}</span>
          <span className="w-1 h-1 bg-gray-600 rounded-full"></span>
          <span className="text-xs bg-gray-800 text-gray-400 px-2 py-1 rounded border border-gray-700">
             {stats.userRole}
          </span>
        </p>
      </header>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Card 1 */}
        <div className="bg-[#1F2937] p-6 rounded-xl border border-gray-700 hover:border-gray-600 transition-colors shadow-sm group">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Total Projects</h3>
            <div className="p-2 bg-blue-500/10 rounded-lg group-hover:bg-blue-500/20 transition-colors">
              <FolderKanban className="w-5 h-5 text-blue-400" />
            </div>
          </div>
          <p className="text-3xl font-bold text-gray-100">{stats.projectCount}</p>
          <div className="mt-2 text-xs text-gray-500">Active projects</div>
        </div>
        
        {/* Card 2 */}
        <div className="bg-[#1F2937] p-6 rounded-xl border border-gray-700 hover:border-gray-600 transition-colors shadow-sm group">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Total Tasks</h3>
            <div className="p-2 bg-purple-500/10 rounded-lg group-hover:bg-purple-500/20 transition-colors">
              <CheckSquare className="w-5 h-5 text-purple-400" />
            </div>
          </div>
          <p className="text-3xl font-bold text-gray-100">{stats.taskCount}</p>
          <div className="mt-2 text-xs text-gray-500">Tasks in all projects</div>
        </div>

        {/* Card 3 */}
        <div className="bg-[#1F2937] p-6 rounded-xl border border-gray-700 hover:border-gray-600 transition-colors shadow-sm group">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Workspace</h3>
            <div className="p-2 bg-green-500/10 rounded-lg group-hover:bg-green-500/20 transition-colors">
              <Building2 className="w-5 h-5 text-green-400" />
            </div>
          </div>
          <p className="text-lg font-bold text-gray-100 truncate">Active</p>
          <div className="mt-2 text-xs text-gray-500">Organization ID: {user.organizationId}</div>
        </div>
      </div>

      {stats.projectCount === 0 && (
        <div className="mt-12 text-center p-12 bg-[#1F2937]/50 rounded-xl border border-dashed border-gray-700">
          <FolderKanban className="w-12 h-12 text-gray-600 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-300">No projects yet</h3>
          <p className="text-gray-500 mt-2 max-w-sm mx-auto mb-6">
            Get started by creating your first project in this workspace.
          </p>
          <button 
             onClick={() => navigate("/projects")}
             className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors"
          >
            Create Project
          </button>
        </div>
      )}
    </div>
  );
}